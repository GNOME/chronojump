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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>


//using EncoderPTCapture : ArduinoCapture
public class EncoderPTCaptureManage
{
	private EncoderPTCapture encoderPTCapture;
	private bool finish;
	private bool cancel;
	private bool error;

	private double distance; //units?
	private List<PointF> points_dt_l;
	private List<PointF> points_st_l;
	private List<PointF> points_at_l;

	public EncoderPTCaptureManage (
			EncoderPTCapture encoderPTCapture,
			ref List<PointF> points_dt_l, ref List<PointF> points_st_l, ref List<PointF> points_at_l)
	{
		this.encoderPTCapture = encoderPTCapture;
		this.points_dt_l = points_dt_l;
		this.points_st_l = points_st_l;
		this.points_at_l = points_at_l;
	}

	public bool Init ()
	{
		finish = false;
		cancel = false;
		error = false;

		encoderPTCapture.Reset ();
		if (! encoderPTCapture.CaptureStart ())
			return false;

		return true;
	}

	public void Capture ()
	{
		double timePre = -1;
		double speedPre = -1;
		bool timePreSet = false;
		bool speedPreSet = false;

		while (! finish && ! cancel && ! error)
		{
			if(! encoderPTCapture.BytesToReadEnoughForASample ())
				continue;

			//LogB.Information ("YESREAD");
			// TODO: this will need to be changed to a buffer read like:
			// readBinaryRunEncoder9Bytes ()
			// encoderCapture.Capture
			// EncoderCaptureInertialBackground.CaptureBG
			if(! encoderPTCapture.CaptureSample ())
				cancel = true; //problem reading line (capturing)

			if (encoderPTCapture.CanReadFromList ())
			{
				EncoderPTEvent epte = encoderPTCapture.EncoderPTCaptureReadNext();
				LogB.Information("epte: " + epte.ToString());

				if (! timePreSet)
				{
					timePre = epte.Time;
					timePreSet = true;
					continue;
				}

				double distanceAtThisSample = UtilAll.DivideSafe (epte.Distance, 6.9); //TODO: why 6.9?
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
			}

		}
		encoderPTCapture.Stop ();

		if (finish)
			LogB.Information("finished");
		if (cancel)
			LogB.Information("cancelled");
	}

	public bool Finish {
		set { finish = value; }
	}
	public bool Cancel {
		set { cancel = value; }
	}
}


