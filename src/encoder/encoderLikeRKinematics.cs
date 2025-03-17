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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

// this class will do same as encoder/util.R kinematicsF

public class EncoderLikeRKinematics
{
	// passed variables
	List<double> dis_l;
	double butterworthFreq;

	List<double> disOrig_l; //just to debug
	List<int> time_l;
	List<double> speed_l;
	List<double> accel_l;
	Butterworth bw;

	public EncoderLikeRKinematics (List<double> dis_l, double butterworthFreq)
	{
		this.dis_l = dis_l;
		this.butterworthFreq = butterworthFreq;
		this.disOrig_l = dis_l;

		calculateSpeed ();
		calculateAccel ();
	}

	private void calculateSpeed ()
	{
		bw = new Butterworth (butterworthFreq);
		bw.AddFromList (dis_l);
		bw.Calculate (Butterworth.TimeEnum.MILIS);

		time_l = bw.Times_l;
		speed_l = bw.Y_l;
	}

	private void calculateAccel ()
	{
		accel_l = new List<double> ();
		int window = 1;
		bw = new Butterworth (butterworthFreq);

		for (int i = 0 ; i < speed_l.Count; i ++)
		{
			int pre = i - window;
			int post = i + window;

			if(pre <= 0)
				pre = 0;
			else if(post >= speed_l.Count -1)
				post = speed_l.Count -1;

			accel_l.Add (UtilAll.DivideSafe (speed_l[post] - speed_l[pre],
						time_l[post] - time_l[pre]
						));

			bw.AddSample (i, accel_l[i]);
		}

		bw.Calculate (Butterworth.TimeEnum.MILIS);

		//time_l = bw.Times_l;
		accel_l = bw.Y_l;
	}

	public void WriteToFileDebug (string filename)
	{
                TextWriter writer = File.CreateText (Path.Combine (Path.GetTempPath (), filename));
	        writer.WriteLine (string.Format ("x;yUnfiltered;speed;accel"));
		for (int i = 0; i < speed_l.Count; i ++)
	              writer.WriteLine (string.Format ("{0};{1};{2};{3}",
					      time_l[i], disOrig_l[i], speed_l[i], accel_l[i]));

                writer.Flush();
                writer.Close();
                ((IDisposable)writer).Dispose();

		// check in R like with:
		// Rscript encoderLikeRKinematics.R filename
	}
}

