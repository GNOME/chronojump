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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

//using System.Diagnostics; //Stopwatch

public partial class ChronoJumpWindow
{
	EncoderPTForceCaptureManage eptfcm;

	//passed variables
	EncoderPTForceCapture eptfc;

	private void encoderPTForceCaptureDo (
			EncoderPTForceCapture eptfc,
			bool binaryBuffer,
			string csvURL,
			int seconds)
	{
		this.eptfc = eptfc;

		//runEncoderPulseMessage = "Capture eptfc... please wait";
		LogB.Information("eptfcm start");

		/*
		if (eptfc == null) //|| eptfc.PortName != chronopicRegister.GetSelectedForMode (current_mode).Port)
			//eptfc = new EncoderPTForceCapture (
//			eptfc = new EncoderPTForceCaptureTestsBinary12Num ( 		// works!
			eptfc = new EncoderPTForceCaptureTestsTextEncCountUp ( 		// works!
//			eptfc = new EncoderPTForceCaptureTestsBinaryEncoder5Bytes ( 	// works!
//			eptfc = new EncoderPTForceCaptureTestsBinaryEncoder8Bytes ( 	// works!
					portName);
					//chronopicRegister.GetSelectedForMode (current_mode).Port,
					//preferences.runEncoderPPS);
		*/
		//else if (eptc.RunEncoderPPS != preferences.runEncoderPPS)
		//	eptc.RunEncoderPPS = preferences.runEncoderPPS;

		//need to pass the ref every capture because every capture we do:
		//cairo...Points_xx_l = new List<PointF> ()

		eptfcm = new EncoderPTForceCaptureManage (
				eptfc, csvURL, seconds, binaryBuffer);/*,
				ref cairoGraphRaceAnalyzerPoints_dt_l,
				ref cairoGraphRaceAnalyzerPoints_st_l,
				ref cairoGraphRaceAnalyzerPoints_at_l
				);
				*/

		LogB.Information("eptfcm start do");
		if (eptfcm.Init ())
		{
			//capturingRunEncoder = arduinoCaptureStatus.CAPTURING;
			//runEncoderPulseMessage = capturingMessage;
			eptfcm.Capture ();

			/*
			LogB.Information("eptfcm end");
			runEncoderPulseMessage = "Done! check log";

			runEncoderProcessCancel = true;
			//capturingRunEncoder = arduinoCaptureStatus.STOP;
			*/
		}
	}
}

//using EncoderPTForceCapture : ArduinoCapture
public class EncoderPTForceCaptureManage : EncoderPTCaptureManage
{
	private EncoderPTForceCapture encoderPTForceCapture;
	private string csvURL;
	private int seconds;
	private bool binaryBuffer;

	/*
	private double distance; //units?
	private List<PointF> points_dt_l;
	private List<PointF> points_st_l;
	private List<PointF> points_at_l;
	*/

	public EncoderPTForceCaptureManage (
			EncoderPTForceCapture encoderPTForceCapture, string csvURL, int seconds, bool binaryBuffer)//,
			//ref List<PointF> points_dt_l, ref List<PointF> points_st_l, ref List<PointF> points_at_l)
	{
		this.encoderPTForceCapture = encoderPTForceCapture;
		this.csvURL = csvURL;
		this.seconds = seconds;
		this.binaryBuffer = binaryBuffer;
		/*
		this.points_dt_l = points_dt_l;
		this.points_st_l = points_st_l;
		this.points_at_l = points_at_l;
		*/
	}

	public override bool Init ()
	{
		finish = false;
		cancel = false;
		error = false;

		encoderPTForceCapture.Reset ();
		if (! encoderPTForceCapture.CaptureStart ())
			return false;

		return true;
	}

	//TODO: implement: sendEndCaptureForceSensorFirstCapture

	public override void Capture ()
	{
		/* not used... yet
		double timePre = -1;
		double speedPre = -1;
		bool timePreSet = false;
		bool speedPreSet = false;
		*/

		TextWriter writer = null;
		if (csvURL != "")
		{
			writer = File.CreateText (csvURL);
			writer.WriteLine ("sensor;time;value");
		}

		//Stopwatch stopwatch = new Stopwatch ();
		//stopwatch.Start ();
		//while (! finish && ! cancel && ! error && stopwatch.Elapsed.TotalSeconds < seconds)
		while (! finish && ! cancel && ! error && encoderPTForceCapture.CurrentValue - encoderPTForceCapture.FirstValue < 10000000)
		{
			/*
			if (binaryBuffer && ! encoderPTForceCapture.BytesToReadEnoughForASample ())
				continue;
				*/
			LogB.Information ("NumBytesToRead:");
			LogB.Information (encoderPTForceCapture.NumBytesToRead ().ToString ());
			//if (encoderPTForceCapture.NumBytesToRead () < encoderPTForceCapture.BytesToReadEnoughForASample ()) //not sure because these are bytes, just use 20 now
			if (encoderPTForceCapture.NumBytesToRead () < 20)
				continue;

			//LogB.Information ("YESREAD");
			// TODO: this will need to be changed to a buffer read like:
			// readBinaryRunEncoder9Bytes ()
			// encoderCapture.Capture
			// EncoderCaptureInertialBackground.CaptureBG
			/*
			if (testing && ! encoderPTForceCapture.CaptureSampleForTesting ())
				cancel = true; //problem reading line (capturing)
			if (! testing && ! encoderPTForceCapture.CaptureSample ())
				cancel = true; //problem reading line (capturing)
				*/
			if (! encoderPTForceCapture.CaptureSample ())
				cancel = true; //problem reading line (capturing)
		
			//LogB.Information ("Seconds: " + stopwatch.Elapsed.TotalSeconds.ToString ());

			if (encoderPTForceCapture.CanReadFromList ())
			{
				EncoderPTForceEvent eptfe = encoderPTForceCapture.ReadNext();
				LogB.Information("eptfe: " + eptfe.ToString());

				if (csvURL != "")
					writer.Write (eptfe + "\n");
				//count ++;

				/*
				if (! timePreSet)
				{
					timePre = epte.Time;
					timePreSet = true;
					continue;
				}

				double distanceAtThisSample = UtilAll.DivideSafe (epte.Distance, 6.9); //TODO: why 6.9? this maybe was just to fit distance on race analyzer graph that we were reusing for this testing implmentation
				distance += distanceAtThisSample;

				double speed = UtilAll.DivideSafe (
						distanceAtThisSample, (epte.Time - timePre)) * 1000000;

				if (! speedPreSet)
				{
					speedPre = speed;
					speedPreSet = true;
					continue;
				}

				double accel = UtilAll.DivideSafe(
					(speed - speedPre), (epte.Time/1000000.0 - timePre/1000000.0) );

				timePre = epte.Time;
				speedPre = speed;

				points_dt_l.Add (new PointF (
							UtilAll.DivideSafe(epte.Time, 1000000),
							distance));
				points_st_l.Add (new PointF (
							UtilAll.DivideSafe(epte.Time, 1000000),
							speed));
				points_at_l.Add (new PointF (
							UtilAll.DivideSafe(epte.Time, 1000000),
							accel));
				*/
			}

		}
		encoderPTForceCapture.Stop ();

		if (csvURL != "")
		{
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		}

		if (finish)
			LogB.Information("finished");
		if (cancel)
			LogB.Information("cancelled");
	}
}


