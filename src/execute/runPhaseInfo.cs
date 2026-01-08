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
 * Copyright (C) 2022  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List

//contains for each phase: isContact? startMSInSequence duration
public class RunPhaseInfo
{
	public enum Types { CONTACT, FLIGHT }

	public Types type;
	public double startMSInSequence; //unused right now
	public double duration;
	public int photocell; //for Wichro (non Wichro will be -1)

	public RunPhaseInfo (Types type, double startMSInSequence, double duration, int photocell)
	{
		this.type = type;
		this.startMSInSequence = startMSInSequence;
		this.duration = duration;
		this.photocell = photocell;
	}

	public bool IsContact()
	{
		return type == Types.CONTACT;
	}

	public double Duration {
		get { return duration; }
	}

	public override string ToString()
	{
		return string.Format("photocell: {0}, type: {1}, startMSInSequence: {2}, duration: {3}",
				photocell, type, startMSInSequence, duration);
	}

}

//manage RunPhaseInfo list
public class RunPhaseInfoManage
{
	public bool TrackDoneHasToBeCalledAgain;
	private static List<RunPhaseInfo> list;
	private int checkTime;

	//TCs and TFs before startPos have been added as tracks
	//do not count again in track operations
	private int startPos;
//	private int startPosPhotocell; //photocell at startPos

	public RunPhaseInfoManage (int checkTime)
	{
		this.checkTime = checkTime;

		list = new List<RunPhaseInfo>();
		startPos = 0;
//		startPosPhotocell = -1;
		TrackDoneHasToBeCalledAgain = false;
	}

	/*
	public void Add (RunPhaseInfo rpi)
	{
		list.Add(rpi);
	}
	*/

	public void UpdateListUsing (List<RunPhaseInfo> listCaptureThread)
	{
		for(int i = list.Count; i < listCaptureThread.Count ; i ++)
			list.Add(listCaptureThread[i]);
	}

	/*
	 * check first TF if exists or all TC+TF pairs to see if all are lower than checkTime (eg 300ms)
	 * return true if all are <= checkTime
	 * if ! speedStart (started inside), don't count first contact time
	 */
	public bool IsStartDoubleContact(bool speedStart)
	{
		LogB.Information("At rpim IsStartDoubleContact A");
		int startAt = 0;

		//TC+TF pairs
		RunPhaseInfo tcRPI;
		RunPhaseInfo tfRPI;
		bool firstPair = true;
		for(int i = startAt +1; i < list.Count; i +=2)
		{
			LogB.Information("At rpim IsStartDoubleContact B pre 1");
			tcRPI = (RunPhaseInfo) list[i-1];
			LogB.Information("At rpim IsStartDoubleContact B pre 2");
			tfRPI = (RunPhaseInfo) list[i];

			LogB.Information("At rpim IsStartDoubleContact B");
			if(firstPair && ! speedStart) {
				LogB.Information("At rpim IsStartDoubleContact B 3");
				//if ! speedStart (started inside), don't count first contact time
				if(tfRPI.Duration > checkTime)
					return false;
			}
			else {
				LogB.Information("At rpim IsStartDoubleContact B 4");
				if(tcRPI.Duration + tfRPI.Duration > checkTime)
					return false;
			}
			firstPair = false;
			LogB.Information("At rpim IsStartDoubleContact B 5");
		}
		LogB.Information("At rpim IsStartDoubleContact C");

		return true;
	}

	private int findTracksInThisChunk(int forStartPos)
	{
		int tracks = 0;
		RunPhaseInfo firstRPI = (RunPhaseInfo) list[0];
		if(! firstRPI.IsContact())
			forStartPos ++;

		//i will be tf, i-1 will be tc
		for(int i = forStartPos + 1; i < list.Count; i += 2)
		{
			RunPhaseInfo tcRPI = (RunPhaseInfo) list[i-1];
			RunPhaseInfo tfRPI = (RunPhaseInfo) list[i];

			if(tcRPI.Duration + tfRPI.Duration > checkTime)
				tracks ++;
		}

		return tracks;
	}

	//find the position of the first big tf of the chunk
	private int findTfPosOfChunk(int forStartPos)
	{
		RunPhaseInfo firstRPI = (RunPhaseInfo) list[0];
		if(! firstRPI.IsContact())
			forStartPos ++;

		//i will be tf, i-1 will be tc
		int i;
		for(i = forStartPos + 1; i < list.Count; i += 2)
		{
			RunPhaseInfo tcRPI = (RunPhaseInfo) list[i-1];
			RunPhaseInfo tfRPI = (RunPhaseInfo) list[i];

			LogB.Information(string.Format("at findTfPosOfChunk: i:{0}, tc:{1}, tf:{2}",
						i, tcRPI, tfRPI));
			if(tcRPI.Duration + tfRPI.Duration > checkTime)
			{
				LogB.Information("YES!");
				return i;
			}
		}

		//we are supposed to not arrive here
		return forStartPos;
	}


	public int GetPosOfBiggestTC (bool started)
	{
		LogB.Information(string.Format("startPos at GetPosOfBiggestTC: {0}, started: {1}", startPos, started));
		TrackDoneHasToBeCalledAgain = false;

		//Read below message: "Message oneTCAfterTheTf"
		if(countTCs() == 1 && oneTCAfterTheTf())
			return startPos +1;

		double max = 0;
		int posBiggest = 0;
		double lastTcDuration = 0;

		/*
		 * first time we need to know if first TC is greater than the others
		 * but once started, we care for endings of each track,
		 * do not use the first value because it's the TC of previous track
		 */
		int forStartPos;
		if(started)
			forStartPos = startPos +1;
		else
			forStartPos = startPos;

		LogB.Information("forStartPos A: " + forStartPos.ToString());

		int tracks = findTracksInThisChunk(forStartPos);
		LogB.Information("findTracksInThisChunk tracks: " + tracks.ToString());

		//on track starts, maybe there are some tc+tf pairs before the big tf
		//A is the track start
		//B is the big tf, we should find biggest tc after this tf
		// A   __   ___B                      __  ___
		if(tracks >= 1)
		{
			forStartPos = findTfPosOfChunk(forStartPos);
			//note forStartPos has changed and following findTfPosOfChunk will start from this tf
		}

		LogB.Information("forStartPos B: " + forStartPos.ToString());

		//this will be the pos of the tf of second Track if exists
		int forEnds = list.Count;
		if(tracks >= 2)
		{
			forEnds = findTfPosOfChunk(forStartPos);
			TrackDoneHasToBeCalledAgain = true;
		}

		LogB.Information("forEnds: " + forEnds.ToString());

		for(int pos = forStartPos; pos < forEnds; pos ++)
		{
			RunPhaseInfo rpi = (RunPhaseInfo) list[pos];

			LogB.Information("rpi: " + rpi.ToString());
			/*
			 * record tc duration as lastTcDuration and add to tf duration to see if is greater than checktime
			 * this allows to return biggest_tc of one track without messing with next track that maybe is captured
			 * this happens because double contacts is eg: 300 and trackDone is calle at 300 * 1,5
			 * But then trackDone has to be called again!
			 */
			if(rpi.IsContact())
				lastTcDuration = rpi.Duration;

			//record posBiggest position
			if(rpi.IsContact() && rpi.Duration > max)
			{
				max = rpi.Duration;
				posBiggest = pos;
			}
		}

		return posBiggest;
	}

	//if pos == -1 return all
	public double SumUntilPos(int pos, bool firstTrackDone, bool speedStart, bool speedStartArrival)
	{
		LogB.Information(string.Format("SumUntilPos: startAt: {0}, until pos: {1}, firstTrackDone: {2}, speedStartArrival: {3}",
					startPos, pos, firstTrackDone, speedStartArrival));

		int countStart = 0;
		double sum = 0;
		string strSum = "";

		int countEnd = 0;
		string plusSign = "";
		foreach(RunPhaseInfo rpi in list)
		{
			if(countStart >= startPos && countEnd < pos)
			{
				/*
				 * if it has not firstTrackDone 1st track take care of leaving or not to count the related tc)
				 * do not count it if
				 *  	started inside (! speedStart) or
				 *  	speed start but start on leaving
				 */
				if( ! firstTrackDone && sum == 0 && rpi.IsContact() && (! speedStart || ! speedStartArrival) )
				{
					//do nothing
				}
				else {
					sum += rpi.Duration;

					//debug
					strSum += string.Format("{0}{1}", plusSign, rpi.Duration);
					plusSign = " + ";
				}
			}

			countStart ++;
			countEnd ++;
		}

		LogB.Information("SumUntilPosProcess: " + strSum);

		return sum;
	}

	public void UpdateStartPos (int bigTCPosition)
	{
		/*
		 * bigTCPosition is the pos of the tc that cut the track.
		 * This tc has to be added on next track
		 */

		startPos = bigTCPosition;
	}

	public int GetPhotocellAtStartPos ()
	{
		if(startPos >= list.Count)
			return -1;

		return ((RunPhaseInfo) list[startPos]).photocell;
	}

	public string PrintList()
	{
		string str = "\n";
		int count = 0;
		foreach(RunPhaseInfo rpi in list)
			str += "\n" + (count ++).ToString() + ": " + rpi.ToString();

		return str;
	}

	//note it starts at startPos
	private int countTCs()
	{
		int countStart = 0;
		int count = 0;
		foreach(RunPhaseInfo rpi in list)
			if(countStart ++ >= startPos && rpi.IsContact())
				count ++;

		return count;
	}

	/*
	 * "Message oneTCAfterTheTf"
	 * if in first track there's only one TC, take care because maybe it has been after the TF
	 * it can happen because tc will be lower than the margin: 300 ms (checktime) + 1.5 * checktime
	 * so first will be the TF, then waiting margin... but TC happens, and then track is processed, track should not include this tf
	 */
	private bool oneTCAfterTheTf()
	{
		if(list.Count - startPos != 2)
			return false;

		RunPhaseInfo first = (RunPhaseInfo) list[0];
		RunPhaseInfo second = (RunPhaseInfo) list[1];

		//check if firt is TF and second TC
		if(! first.IsContact() && second.IsContact())
			return true;

		return false;
	}

	public int LastPositionOfList {
		get { return list.Count -1; }
	}

	//to debug
	public int StartPos {
		get { return startPos; }
	}
}
