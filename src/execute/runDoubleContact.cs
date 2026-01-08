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

//manage double contacts in runs
public class RunDoubleContact
{
	public bool SpeedStart; 	//comes with speed or started in contact with the photocell
	public bool FirstTrackDone; 	//the manage of speedStartArrival has been done
	public bool TrackDoneHasToBeCalledAgain;

	private Constants.DoubleContact mode;
	private int checkTime;
	private bool speedStartArrival;

	private RunPhaseInfoManage rpim;

	//these are used also to know track time if there are no double contacts
	private double lastTc; //important to check lastTc and currentTF to measure if they are above or not checkTime
	
	private double timeAcumulated;

	private List<RunPhaseInfo> listCaptureThread; //this list contains TCs and TFs from capture thread


	//constructor ------------------------------------------
	public RunDoubleContact (Constants.DoubleContact mode, int checkTime, bool speedStartArrival)
	{
		this.mode = mode;
		this.checkTime = checkTime;
		this.speedStartArrival = speedStartArrival;

		lastTc = 0;
		timeAcumulated = 0;
		rpim = new RunPhaseInfoManage(checkTime);
		listCaptureThread = new List<RunPhaseInfo>();
		FirstTrackDone = false;
		TrackDoneHasToBeCalledAgain = false;
	}

	//public methods ---------------------------------------

	/*
	 * ---------------------- start of called by capture thread -------------->
	 */

	public bool UseDoubleContacts ()
	{
		return (mode != Constants.DoubleContact.NONE);
	}

	public void DoneTC (double timestamp, bool timeStarted, int photocell)
	{
		LogB.Information("DONETC timestamp: " + timestamp + timestamp.ToString());
		lastTc = timestamp;
		listCaptureThread.Add(new RunPhaseInfo(RunPhaseInfo.Types.CONTACT, timeAcumulated, timestamp, photocell));
		if(timeStarted)
			timeAcumulated += timestamp;

		LogB.Information(string.Format("DoneTC -> lastTc: {0}", lastTc));
	}

	public void DoneTF (double timestamp, int photocell)
	{
		LogB.Information("DONETF timestamp: " + timestamp + timestamp.ToString());
		LogB.Information(string.Format(
					"lastTc + timestamp <= checkTime ?, lastTc: {0}; timestamp: {1}; checkTime: {2}",
					lastTc, timestamp, checkTime));

		listCaptureThread.Add(new RunPhaseInfo(RunPhaseInfo.Types.FLIGHT, timeAcumulated, timestamp, photocell));
		timeAcumulated += timestamp;
	}

	/*
	 * <---------------------- end of called by capture thread --------------
	 */

	/*
	 * ---------------------- start of called by GTK thread ---------------->
	 */

	//Copies from listWill to list
	public void UpdateList()
	{
		rpim.UpdateListUsing (listCaptureThread);
	}

	public bool IsStartDoubleContact()
	{
		LogB.Information("At RunDC IsStartDoubleContact");

		bool isDC = rpim.IsStartDoubleContact(SpeedStart);
		LogB.Information("IsStartDoubleContact: " + isDC.ToString());
		return isDC;
	}

	public int GetPosOfBiggestTC(bool started)
	{
		int pos = rpim.GetPosOfBiggestTC(started);

		if(rpim.TrackDoneHasToBeCalledAgain)
		{
			TrackDoneHasToBeCalledAgain = true;
			//rpim.TrackDoneHasToBeCalledAgain = false;
		}

		LogB.Information(string.Format("GetPosOfBiggestTC list: {0}, pos: {1}, hasToBeCalledAgain: {2}",
					rpim.PrintList(), pos, TrackDoneHasToBeCalledAgain));

		return pos;
	}

	//this wait will be done by C#
	public double GetTrackTimeInSecondsAndUpdateStartPos()
	{
		double trackTime = getDCBiggestTC();

		//in seconds
		if(trackTime > 0)
			trackTime /= 1000.0;

		return trackTime;
	}

	public void UpdateStartPos(int newPos)
	{
		rpim.UpdateStartPos(newPos);
	}

	public int GetPhotocellAtStartPos () //TODO; or maybe before
	{
		return rpim.GetPhotocellAtStartPos();
	}

	/*
	 * <---------------------- end of called by GTK thread --------------
	 */

	//private methods --------------------------------------
	
	private double getDCBiggestTC()
	{
		int bigTCPosition = GetPosOfBiggestTC(true);
		double sum = rpim.SumUntilPos(bigTCPosition, FirstTrackDone, SpeedStart, speedStartArrival);
		LogB.Information(string.Format("trackDoing getDCBiggestTC bigTCPosition: {0}, Sum: {1}", bigTCPosition, sum));

		//fix problem of a tc + tf lower than checkTime
		if(sum < checkTime)
		{
			while (sum < checkTime && bigTCPosition +2 <= rpim.LastPositionOfList)
			{
				bigTCPosition += 2;
				sum = rpim.SumUntilPos(bigTCPosition, FirstTrackDone, SpeedStart, speedStartArrival);
				LogB.Information(string.Format("SUM was < checkTime. New bigTCPosition: {0}, New Sum: {1}", bigTCPosition, sum));
			}
		}

		UpdateStartPos(bigTCPosition);

		return sum;
	}
}
