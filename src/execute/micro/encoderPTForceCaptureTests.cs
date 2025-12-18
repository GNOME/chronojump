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
 * Copyright (C) 2025  Xavier de Blas <xaviblas@gmail.com>
 */

using System;

public abstract class EncoderPTForceCaptureTests: EncoderPTForceCapture
{
	protected string portName;

	public override bool CaptureSample ()
	{
		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
			return false;

		if(! readSample ())
		{
			micro.ClosePort ();
			return false;
		}

		return true;
	}
}

public class EncoderPTForceCaptureTestsBinary12Num: EncoderPTForceCaptureTests
{
	public EncoderPTForceCaptureTestsBinary12Num (string portName)
	{
		this.portName = portName;

		cancel = false;
		startCaptureStr = "start_capture_binary12Num:";
		bufferBinaryBytesToReadAtLeast = 12;

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}
	
	protected override bool readSample ()
	{
		micro.BufferInit ();
		int bytesRead = 0;
		try {
			bytesRead = micro.ReadWithBuffer (0, 12); //is this 12 bytes buffer read useful at all?
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
}

// this printed sucessfully 19880 records in 10 s
public class EncoderPTForceCaptureTestsTextEncCountUp: EncoderPTForceCaptureTests
{
	private int debugTextCount;

	public EncoderPTForceCaptureTestsTextEncCountUp (string portName)
	{
		this.portName = portName;

		cancel = false;
		startCaptureStr = "start_capture_textEncCountUp:";
		bufferBinaryBytesToReadAtLeast = -1; // unused

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();

		// for buffer on text transmission
		micro.BufferInit ();
		bufferRemainingStr = "";

		debugTextCount = -1; //count it does not start at 0, as maybe we will start receiving when Arduino has sended lot of info
	}

	protected override bool readSample ()
	{
		int bytesRead = micro.ReadWithBuffer (0, -1);

		//string s = "";
		string s = bufferRemainingStr;
		LogB.Information (string.Format ("s before for:|{0}|", s));
		for (int i = 0; i < bytesRead; i ++)
		{
			int c = micro.GetBufferAtPos (i);
			if (c == 59)
				s += ";";
			else if (c == 10 || c == 13) // 10: line feed; 13: carriage return
			{
				if (s != "") //to not show empty sample when linefeed & carriage return
				{
					//LogB.Information ("sample: " + s);
					if (! processSample (s))
						return false;
				}
				s = "";
			} else
				s += micro.GetBufferAtPos (i) - '0';
		}
		bufferRemainingStr = s;

		LogB.Information (string.Format ("bufferRemainingStr:|{0}|", bufferRemainingStr));
		return true;
	}

	string bufferRemainingStr; //to store chars not processed, part of the next sample that will be readed on next ReadWithBuffer

	private bool processSample (string s)
	{
		string [] sFull = s.Split(new char[] {';'});
		if (sFull.Length != 2)
		{
			LogB.Information ("error 1 - received: " + s);
			return false;
		}
		
		if (! Util.IsNumber (sFull[0], false))
		{
			LogB.Information ("error 2 - received: " + s);
			return false;
		}

		int debugTextCountReceived = Convert.ToInt32 (sFull[0]);
		if (debugTextCount >= 0 && debugTextCountReceived != debugTextCount +1)
		{
		LogB.Information (string.Format (
					"error 3 - received: {0}, debugTextCount: {1}, debugTextCountRecieved: {2}",
					s, debugTextCount, debugTextCountReceived));
			return false;
		}

		debugTextCount = debugTextCountReceived;
		LogB.Information (string.Format ("received: {0} (count: {1})", s, receivedN ++));

		return true;
	}
}


/*
// this sends 20 relative times and then the diff of this 20
public class EncoderPTForceCaptureTestsTextRelAndSum: EncoderPTForceCaptureTests
{
	private int debugTextCount;

	public EncoderPTForceCaptureTestsTextRelAndSum (string portName)
	{
		this.portName = portName;

		cancel = false;
		startCaptureStr = "start_capture_textEncRelAndSum:";
		bufferBinaryBytesToReadAtLeast = -1; // unused

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}

	protected override bool readSample ()
	{
		micro.BufferInit ();
		int bytesRead = 0;
		try {
			bytesRead = micro.ReadWithBuffer (0, 5); //is this 5 bytes buffer read useful at all?
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinarySampleTesting port.Read ()");

			return false;
		}
	}
}
*/
	
// this use to FAIL at 1500 receivedN aprox
public class EncoderPTForceCaptureTestsBinaryEncoder5Bytes: EncoderPTForceCaptureTests
{
	public EncoderPTForceCaptureTestsBinaryEncoder5Bytes (string portName)
	{
		this.portName = portName;

		cancel = false;
		startCaptureStr = "start_capture_binaryEncoder5Bytes:";
		bufferBinaryBytesToReadAtLeast = 5;

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}

	protected override bool readSample ()
	{
		micro.BufferInit ();
		int bytesRead = 0;
		try {
			bytesRead = micro.ReadWithBuffer (0, 5); //is this 5 bytes buffer read useful at all?
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinarySampleTesting port.Read ()");

			return false;
		}

		int b0, b1, b2, b3, b4;
		int count = 0;

		// sensor type (1 byte)
		b0 = micro.GetBufferAtPos (count ++);
		//LogB.Information ("b0 : " + b0.ToString ());

		if (! Util.IsNumber (b0.ToString (), false))
		{
			LogB.Information (string.Format ("FAIL event is not int: {0}", b0));
			return false;
		}
		if (Convert.ToInt32 (b0) < 4 || Convert.ToInt32 (b0) > 5)
		{
			LogB.Information (string.Format ("FAIL event is not 4 or 5: {0}", b0));
			return false;
		}
		int sensor = Convert.ToInt32 (b0);

		// time (4 bytes)
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);
                b4 = micro.GetBufferAtPos (count ++);
		/*
		LogB.Information ("b1 : " + b1.ToString ());
		LogB.Information ("b2 : " + b2.ToString ());
		LogB.Information ("b3 : " + b3.ToString ());
		LogB.Information ("b4 : " + b4.ToString ());
		*/

		if (! Util.IsUint ((Math.Pow(256,3) * b4 +
					Math.Pow(256,2) * b3 +
					Math.Pow(256,1) * b2 +
					Math.Pow(256,0) * b1).ToString ()))
		{
			LogB.Information (string.Format ("FAIL time is not uint: {0}",
						Math.Pow(256,3) * b4 + Math.Pow(256,2) * b3 +
						Math.Pow(256,1) * b2 + Math.Pow(256,0) * b1));
			return false;
		}
		uint time = Convert.ToUInt32 (Math.Pow(256,3) * b4 + Math.Pow(256,2) * b3 +
			Math.Pow(256,1) * b2 + Math.Pow(256,0) * b1);

		LogB.Information (string.Format ("event: {0}, time: {1}, receivedN: {2}",
					sensor, time, receivedN ++
					));

		return true;
	}
}

// this use to FAIL at 1000 receivedN aprox
public class EncoderPTForceCaptureTestsBinaryEncoder8Bytes: EncoderPTForceCaptureTests
{
	public EncoderPTForceCaptureTestsBinaryEncoder8Bytes (string portName)
	{
		this.portName = portName;

		cancel = false;
		startCaptureStr = "start_capture_binaryEncoder8Bytes:";
		bufferBinaryBytesToReadAtLeast = 8;

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}

	protected override bool readSample ()
	{
		micro.BufferInit ();
		int bytesRead = 0;
		try {
			bytesRead = micro.ReadWithBuffer (0, 8); //is this 8 bytes buffer read useful at all?
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinarySampleTesting port.Read ()");

			return false;
		}

		int b0, b1, b2, b3;//, b4;
		int count = 0;

		// sensor type (4 bytes)
                b0 = micro.GetBufferAtPos (count ++);
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);
		/*
		LogB.Information ("b0 : " + b1.ToString ());
		LogB.Information ("b1 : " + b1.ToString ());
		LogB.Information ("b2 : " + b2.ToString ());
		LogB.Information ("b3 : " + b3.ToString ());
		*/
		if (! Util.IsNumber ((Math.Pow(256,3) * b3 +
					Math.Pow(256,2) * b2 +
					Math.Pow(256,1) * b1 +
					Math.Pow(256,0) * b0).ToString (), false))
		{
			LogB.Information (string.Format ("FAIL sensor number is not int: {0}",
						Math.Pow(256,3) * b3 + Math.Pow(256,2) * b2 +
						Math.Pow(256,1) * b1 + Math.Pow(256,0) * b0));
			return false;
		}
		int sensor = Convert.ToInt32 (Math.Pow(256,3) * b3 + Math.Pow(256,2) * b2 +
			Math.Pow(256,1) * b1 + Math.Pow(256,0) * b0);

		if (sensor < 4 || sensor > 5)
		{
			LogB.Information (string.Format ("FAIL sensor <4 || > 5: {0}", sensor));
			return false;
		}

		// time (4 bytes)
                b0 = micro.GetBufferAtPos (count ++);
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);
		/*
		LogB.Information ("b0 : " + b1.ToString ());
		LogB.Information ("b1 : " + b1.ToString ());
		LogB.Information ("b2 : " + b2.ToString ());
		LogB.Information ("b3 : " + b3.ToString ());
		*/
		if (! Util.IsUint ((Math.Pow(256,3) * b3 +
					Math.Pow(256,2) * b2 +
					Math.Pow(256,1) * b1 +
					Math.Pow(256,0) * b0).ToString ()))
		{
			LogB.Information (string.Format ("FAIL time is not uint: {0}",
						Math.Pow(256,3) * b3 + Math.Pow(256,2) * b2 +
						Math.Pow(256,1) * b1 + Math.Pow(256,0) * b0));
			return false;
		}
		uint time = Convert.ToUInt32 (Math.Pow(256,3) * b3 + Math.Pow(256,2) * b2 +
			Math.Pow(256,1) * b1 + Math.Pow(256,0) * b0);

		LogB.Information (string.Format ("event: {0}, time: {1}, receivedN: {2}",
					sensor, time, receivedN ++
					));

		return true;
	}
}
