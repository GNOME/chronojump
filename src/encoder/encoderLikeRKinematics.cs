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
	List<int> time_l;
	List<double> speed_l;
	List<PointF> pointF_l;

	List<double> disOrig_l; //just to debug

	public EncoderLikeRKinematics (List<double> dis_l, double butterworthFreq)
	{
		this.disOrig_l = dis_l;

		time_l = new List<int>();
		speed_l = new List<double>();
		pointF_l = new List<PointF> ();

		Butterworth bw = new Butterworth (butterworthFreq);
		
		for (int i = 0; i < dis_l.Count ; i ++)
			pointF_l.Add (new PointF (i, dis_l[i]));
		bw.AddFromList (pointF_l);

		bw.Calculate (Butterworth.TimeEnum.MILIS);
		time_l = bw.Times_l;
		speed_l = bw.Y_l;
		pointF_l = bw.PointF_l;
	}

	public void WriteToFileDebug (string filename)
	{
                TextWriter writer = File.CreateText (Path.Combine (Path.GetTempPath (), filename));
		int i = 0;
	        writer.WriteLine (string.Format ("x;yFiltered;yUnfiltered"));
		foreach (PointF p in pointF_l)
	                writer.WriteLine (string.Format ("{0};{1};{2}", p.X, p.Y, disOrig_l[i++]));

                writer.Flush();
                writer.Close();
                ((IDisposable)writer).Dispose();

		/* check in R like this:
		 * d=read.csv2("encoderDebug_XXXX.txt") #substitute XXXX by the startInSet value
		 * plot (cumsum(d$yUnfiltered), type="l", main = "Eencoder pos")
		 * lines (cumsum(d$yFiltered)+10, type="l", col="red")
		 * legend ("bottomright", lty=1, col=c("black","red"), c("No filter", "Bw 15 (+10y)"))
		 */
	}
}

