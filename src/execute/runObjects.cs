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
 * Copyright (C) 2018  Xavier de Blas <xaviblas@gmail.com> 
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

	public RunPhaseInfo (Types type, double startMSInSequence, double duration)
	{
		this.type = type;
		this.startMSInSequence = startMSInSequence;
		this.duration = duration;
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
		return string.Format("type: {0}, startMSInSequence: {1}, duration: {2}",
				type, startMSInSequence, duration);
	}

}
//manage RunPhaseInfo list
public class RunPhaseInfoManage
{
	private List<RunPhaseInfo> list;

	//TCs and TFs before startPos have been added as tracks
	//do not count again in track operations
	private int startPos;

	public RunPhaseInfoManage ()
	{
		list = new List<RunPhaseInfo>();
		startPos = 0;
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

	public int GetPosOfBiggestTC ()
	{
		LogB.Information("startPos at GetPosOfBiggestTC: " + startPos.ToString());

		//Read below message: "Message oneTCAfterTheTf"
		if(countTCs() == 1 && oneTCAfterTheTf())
			return 1;

		//if there's no tc, return -1, track duration will be tf duration
		//if there's one tc, return -1, track duration will be tc+tf duration
		if(countTCs() < 2)
			return -1;

		//if there's more than one tc, return the pos of bigger tc,
		//but first tc cannot be the biggest, because first tc will be always added (and first tf)
		double max = 0;
		int pos = 0;
		int posBiggest = 0;
		foreach(RunPhaseInfo rpi in list)
		{
			//pos > startPos +1 to not allow first tc to be the biggest
			if(pos > startPos +1 && rpi.IsContact() && rpi.Duration > max)
			{
				max = rpi.Duration;
				posBiggest = pos;
			}

			pos ++;
		}
		return posBiggest;
	}

	//if pos == -1 return all
	public double SumUntilPos(int pos)
	{
		int countStart = 0;
		double sum = 0;

		if(pos == -1)
		{
			foreach(RunPhaseInfo rpi in list)
				if(countStart ++ >= startPos)
					sum += rpi.Duration;
		} else {
			int countEnd = 0;
			foreach(RunPhaseInfo rpi in list)
			{
				if(countStart >= startPos && countEnd < pos)
					sum += rpi.Duration;

				countStart ++;
				countEnd ++;
			}
		}

		return sum;
	}

	public void UpdateStartPos (int bigTCPosition)
	{
		//if bigTCPosition == -1 , startPos will be one element more than list
		//else bigTCPosition is the pos of the tc that cut the track. This tc has to be added on next track

		if(bigTCPosition == -1)
			startPos = list.Count;
		else
			startPos = bigTCPosition;
	}

	public string PrintList()
	{
		string str = "\n";
		foreach(RunPhaseInfo rpi in list)
			str += "\n" + rpi.ToString();

		return str;
	}

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
		if(list.Count != 2)
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

//manage double contacts in runs
public class RunDoubleContact
{
	private Constants.DoubleContact mode;
	private int checkTime;

	private RunPhaseInfoManage rpim;

	//these are used also to know track time if there are no double contacts
	private double lastTc; //important to check lastTc and currentTF to measure if they are above or not checkTime
	
	private double timeAcumulated;

	private List<RunPhaseInfo> listCaptureThread; //this list contains TCs and TFs from capture thread

	//constructor ------------------------------------------
	public RunDoubleContact (Constants.DoubleContact mode, int checkTime)
	{
		this.mode = mode;
		this.checkTime = checkTime;

		lastTc = 0;
		timeAcumulated = 0;
		rpim = new RunPhaseInfoManage();
		listCaptureThread = new List<RunPhaseInfo>();
	}

	//public methods ---------------------------------------

	/*
	 * ---------------------- start of called by capture thread -------------->
	 */

	public bool UseDoubleContacts ()
	{
		return (mode != Constants.DoubleContact.NONE);
	}

	public void DoneTC (double timestamp)
	{
		lastTc = timestamp;
		listCaptureThread.Add(new RunPhaseInfo(RunPhaseInfo.Types.CONTACT, timeAcumulated, timestamp));
		timeAcumulated += timestamp;
		LogB.Information(string.Format("DoneTC -> lastTc: {0}", lastTc));
	}

	public void DoneTF (double timestamp)
	{
		LogB.Information(string.Format(
					"lastTc + timestamp <= checkTime ?, lastTc: {0}; timestamp: {1}; checkTime: {2}",
					lastTc, timestamp, checkTime));

		listCaptureThread.Add(new RunPhaseInfo(RunPhaseInfo.Types.FLIGHT, timeAcumulated, timestamp));
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

	//this wait will be done by C#
	public double GetTrackTimeInSecondsAndUpdateStartPos()
	{
		double trackTime = 0;
		/*
		if(mode == Constants.FoubleContact.FIRST)
			timestamp = getDCFirst();
		else if(mode == Constants.FoubleContact.LAST)
			timestamp = getDCLast();
		else if(mode == Constants.FoubleContact.AVERAGE)
			timestamp = getDCAverage();
		else // if(mode == Constants.FoubleContact.BIGGEST_TC)
		*/
			trackTime = getDCBiggestTC(); //superhardcoded

		//in seconds
		if(trackTime > 0)
			trackTime /= 1000.0;

		return trackTime;
	}

	/*
	 * <---------------------- end of called by GTK thread --------------
	 */

	//private methods --------------------------------------
	
	private double getDCBiggestTC()
	{
		int bigTCPosition = rpim.GetPosOfBiggestTC();
		double sum = rpim.SumUntilPos(bigTCPosition);

		LogB.Information(string.Format("\n----------------\ngetDCBiggestTC, list: {0}, startPos: {1}, bigTCPosition: {2}, sum: {3}",
					rpim.PrintList(), rpim.StartPos, bigTCPosition, sum));

		//fix problem of a tc + tf lower than checkTime
		if(sum < checkTime)
		{
			while (sum < checkTime && bigTCPosition +2 <= rpim.LastPositionOfList)
			{
				bigTCPosition += 2;
				sum = rpim.SumUntilPos(bigTCPosition);
				LogB.Information(string.Format("SUM was < checkTime. New bigTCPosition: {0}, New Sum: {1}", bigTCPosition, sum));
			}
		}

		//rpim.EmptyListUntilPos(bigTCPosition);
		rpim.UpdateStartPos(bigTCPosition);

		return sum;
	}
}

//decide if use this or inspector
public class RunPhaseTimeList
{
	private List<PhaseTime> listPhaseTime;

	public RunPhaseTimeList()
	{
		listPhaseTime = new List<PhaseTime>();
	}
	
	public void AddTC(double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(true, timestamp));
	}

	public void AddTF(double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(false, timestamp));
	}

	public override string ToString()
	{
		string str = "";

		//This is problematic (Collection was modified; enumeration operation may not execute) if other thread is changing it:
		//foreach(PhaseTime pt in listPhaseTime)
		//solution:
		List<PhaseTime> listPhaseTimeShallowCloned = new List<PhaseTime>(listPhaseTime);
		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
			str += pt.ToString();

		return str;
	}

	public List<string> InListForPainting()
	{
		List<string> list_in = new List<string>();
		int currentMS = 0;
		int startInMS = -1;

		//This is problematic (Collection was modified; enumeration operation may not execute) if other thread is changing it:
		//foreach(PhaseTime pt in listPhaseTime)
		//solution:
		List<PhaseTime> listPhaseTimeShallowCloned = new List<PhaseTime>(listPhaseTime);
		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
		{
			if(pt.IsContact)
				startInMS = currentMS;
			else if(startInMS >= 0)
				list_in.Add(startInMS/1000.0 + ":" + currentMS/1000.0); //in seconds

			currentMS += Convert.ToInt32(pt.Duration);
		}

		return list_in;
	}

	//Debug
	public string InListForPaintingToString()
	{
		string str = "Contact in time list:\n";
		List<string> list_in = InListForPainting();
		foreach(string s in list_in)
			str += s + "\n";

		return str;
	}

}

//currently used for simple runs
public class RunExecuteInspector
{
	public enum Types { RUN_SIMPLE, RUN_INTERVAL }
	private Types type;

	public enum Phases { START, IN, OUT, END }
	private DateTime dtStarted;
	private DateTime dtEnded;

	private bool speedStartArrival;
	Constants.DoubleContact checkDoubleContactMode;
	int checkDoubleContactTime;

	private List<InOut> listInOut;


	//constructor
	public RunExecuteInspector(Types type, bool speedStartArrival,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime)
	{
		this.type = type;
		this.speedStartArrival = speedStartArrival;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;

		listInOut = new List<InOut>();
	}

	//public methods

	public void ChangePhase(Phases phase)
	{
		ChangePhase(phase, "");
	}
	public void ChangePhase(Phases phase, string message)
	{
		DateTime dt = DateTime.Now;

		if(phase == Phases.START)
			dtStarted = dt;
		else if(phase == Phases.END)
			dtEnded = dt;
		else // (phase == Phases.IN || phases == Phases.OUT)
		{
			InOut inOut = new InOut(phase == Phases.IN, dt, message);
			listInOut.Add(inOut);
			//listInOut.Add(new InOut(phase == Phases.IN, dt, message));
		}
	}

	public override string ToString()
	{
		string report = string.Format("RunExecuteInspector RunEI report of race started: {0}; ended: {1}", dtStarted.ToShortTimeString(), dtEnded.ToShortTimeString());
		report += "\n" + "Type: " + type.ToString();
		report += "\n" + "SpeedStartArrival: " + speedStartArrival;
		report += "\n" + "CheckDoubleContactMode: " + checkDoubleContactMode;
		report += "\n" + "CheckDoubleContactTime: " + checkDoubleContactTime;
		report += "\n" + "Chronopic changes:";
		foreach(InOut inOut in listInOut)
		{
			report += inOut.ToString();
		}
		return report;
	}
}
