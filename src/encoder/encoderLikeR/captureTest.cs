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

//this class opens data recorded by encoder but processes it like capture to test differences on Butterworth filtering
//
public class EncoderLikeRCaptureTest
{
	string url = "/home/xavier/.local/share/Chronojump/encoder/152/data/signal/192-Giles-2015-03-06_13-19-58.txt";

	public EncoderLikeRCaptureTest ()
	{
		LogB.Information ("EncoderLikeRCaptureTest ()");
		// 1 read signal file
		if (! Util.FileExists (url))
			return;

		int [] dis = Util.ReadFileAsInts (url);
		//note above method reads 0 as 48, 1 as 49, -1 as -49, -2 as -50, so:
		for (int i = 0; i < dis.Count(); i ++)
		{
			//LogB.Information (dis[i].ToString ());
			if (dis[i] > 0)
				dis[i] -= 48; //48, 49 to 0, 1
			else 
				dis[i] += 48; //-49, -50 to -1, -2
		}

		List<int> dis_l = UtilList.IntArrayToListInt (dis);

		// 2 create econf and encoderParams
		EncoderConfiguration econf = new EncoderConfiguration (
				EncoderConfiguration.Names.ROTARYAXISINERTIAL);
		string[] strFull = "ROTARYAXISINERTIAL:4:-1:-1:-1:101:1:363:3:660:11,5:4".Split(new char[] { ':' });
		econf.ReadParamsFromSQL (strFull);

		EncoderParams encoderParams = new EncoderParams (
				20, 0, "75", "0",
				"ecS", "-", "none", "p",
				true, 4,
				econf,
				"0.7", 0, 0, 0, "POINT");

		// 3 instantiate EncoderLikeRCapture processing all signal at once

		LogB.Information ("EncoderLikeRCaptureTest () 1");
		EncoderLikeRCapture elrc = new EncoderLikeRCapture (encoderParams);

		elrc.Do (true, true, dis_l, dis_l, 0, 0, 0, 0);

		elrc.Kinematics.WriteToFileDebug ("debugAll.csv");

		// 4 cut & process individually each of the reps
		/* loading file with R and running extrema:
		> extrema(pos)
		$minindex
		      [,1]  [,2]
		[1,]  1451  1808
		[2,]  5428  5464
		[3,]  8570  8587
		[4,] 11065 11087
		[5,] 13584 13620
		
		$maxindex
		      [,1]  [,2]
		[1,]  1809  2899
		[2,]  7278  7310
		[3,]  9852  9871
		[4,] 12234 12250
		
		$nextreme
		[1] 9
		
		$cross
		      [,1]  [,2]
		[1,]     1    99
		[2,]  6252  6253
		[3,]  7918  7919
		[4,]  9235  9236
		[5,] 10455 10455
		[6,] 11671 11671
		[7,] 12852 12852
		[8,] 17284 17286
		
		$ncross
		[1] 8
		*/

		/*
		List<EncoderLikeRRepetition> rep_l = new List<EncoderLikeRRepetition> {
			//TODO: need also 1st ecc
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((5428+5464)/2), 6252),
			
			//new EncoderLikeRRepetition (
			//		Convert.ToInt32 ((5428+5464)/2)-1000, 6252+1000), //same but 1s bigger each side
			//TODO: do less than 1 second, there's no need of so much
			//new EncoderLikeRRepetition (
			//		Convert.ToInt32 ((5428+5464)/2)-100, 6252+100), //same but 100ms bigger each side
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((5428+5464)/2)-200, 6252+200), //same but 200ms bigger each side
			//new EncoderLikeRRepetition (
			//		Convert.ToInt32 ((5428+5464)/2)-500, 6252+500), //same but 500ms bigger each side

			new EncoderLikeRRepetition (
					6253, Convert.ToInt32 ((7278+7310)/2)),
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((7278+7310)/2), 7918),
			new EncoderLikeRRepetition (
					7919, Convert.ToInt32 ((8570+8587)/2)),
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((8570+8587)/2), 9235),
			new EncoderLikeRRepetition (
					9236, Convert.ToInt32 ((9852+9871)/2)),
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((9852+9871)/2), 10455),
			new EncoderLikeRRepetition (
					10455, Convert.ToInt32 ((11065+11087)/2)),
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((11065+11087)/2), 11671),
			new EncoderLikeRRepetition (
					11671, Convert.ToInt32 ((12234+12250)/2)),
			new EncoderLikeRRepetition (
					Convert.ToInt32 ((12234+12250)/2), 12852),
			new EncoderLikeRRepetition (
					12852, Convert.ToInt32 ((13584+13620)/2))
		};

		foreach (EncoderLikeRRepetition rep in rep_l)
		{
			List<int> disRep_l = UtilList.ListGetFromToIncluded (dis_l, rep.start, rep.end);

			elrc = new EncoderLikeRCapture (encoderParams);
			elrc.Do (true, true, disRep_l, disRep_l, 0, 0);
			elrc.Kinematics.WriteToFileDebug (string.Format ("debug_{0}.csv", rep.start));
		}
		*/

			//trying 1st concentric repetition
			EncoderLikeRRepetition repCon1 = new EncoderLikeRRepetition (
					Convert.ToInt32 ((5428+5464)/2), 6252);
			EncoderLikeRRepetition repCon1Smooth = new EncoderLikeRRepetition (
					Convert.ToInt32 ((5428+5464)/2)-200, 6252+200); //same but 200ms bigger each side

			elrc = new EncoderLikeRCapture (encoderParams);
			elrc.Do (true, true,
					UtilList.ListGetFromToIncluded (dis_l, repCon1.start, repCon1.end),
					UtilList.ListGetFromToIncluded (dis_l, repCon1Smooth.start, repCon1Smooth.end), 200, 200,
					0, 0);
			elrc.Kinematics.WriteToFileDebug (string.Format ("debug_{0}.csv", repCon1.start));


			//trying 2nd ecccentric repetition
			EncoderLikeRRepetition repEcc2 = new EncoderLikeRRepetition (
					6253, Convert.ToInt32 ((7278+7310)/2));
			EncoderLikeRRepetition repEcc2Smooth = new EncoderLikeRRepetition (
					6253-200, Convert.ToInt32 ((7278+7310)/2)+200); //same but 200ms bigger each side

			elrc = new EncoderLikeRCapture (encoderParams);
			elrc.Do (true, true,
					UtilList.ListGetFromToIncluded (dis_l, repEcc2.start, repEcc2.end),
					UtilList.ListGetFromToIncluded (dis_l, repEcc2Smooth.start, repEcc2Smooth.end), 200, 200,
					0, 0);
			elrc.Kinematics.WriteToFileDebug (string.Format ("debug_{0}.csv", repEcc2.start));
	}
}
