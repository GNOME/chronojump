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

	public RunPhaseInfoManage ()
	{
		list = new List<RunPhaseInfo>();
	}
		
	public void Add (RunPhaseInfo rpi)
	{
		list.Add(rpi);
	}

	public int GetPosOfBiggestTC ()
	{
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
			if(pos > 0 && rpi.IsContact() && rpi.Duration > max) //pos > 0 to not allow first tc to be the biggest
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
		double sum = 0;

		if(pos == -1) {
			foreach(RunPhaseInfo rpi in list)
				sum += rpi.Duration;
		} else {
			int count = 0;
			foreach(RunPhaseInfo rpi in list)
				if(count ++ < pos)
					sum += rpi.Duration;
		}

		return sum;
	}
	
	//if pos == -1 empty all
	public void EmtpyListUntilPos(int pos)
	{
		if(pos == -1)
			list = new List<RunPhaseInfo>();
		else {
			List<RunPhaseInfo> listNew = new List<RunPhaseInfo>();
			int count = 0;
			foreach(RunPhaseInfo rpi in list)
				if(count ++ >= pos)
					listNew.Add(rpi);

			list = listNew;
		}
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
		int count = 0;
		foreach(RunPhaseInfo rpi in list)
			if(rpi.IsContact())
				count ++;

		return count;
	}

	public int LastPositionOfList {
		get { return list.Count -1; }
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

	//constructor ------------------------------------------
	public RunDoubleContact (Constants.DoubleContact mode, int checkTime)
	{
		this.mode = mode;
		this.checkTime = checkTime;

		lastTc = 0;
		timeAcumulated = 0;
		rpim = new RunPhaseInfoManage();
	}

	//public methods ---------------------------------------

	public bool UseDoubleContacts ()
	{
		return (mode != Constants.DoubleContact.NONE);
	}

	public void DoneTC (double timestamp)
	{
		lastTc = timestamp;
		rpim.Add(new RunPhaseInfo(RunPhaseInfo.Types.CONTACT, timeAcumulated, timestamp));
		timeAcumulated += timestamp;
		LogB.Information(string.Format("DoneTC -> lastTc: {0}", lastTc));
	}

	public void DoneTF (double timestamp)
	{
		LogB.Information(string.Format(
					"lastTc + timestamp <= checkTime ?, lastTc: {0}; timestamp: {1}; checkTime: {2}",
					lastTc, timestamp, checkTime));

		rpim.Add(new RunPhaseInfo(RunPhaseInfo.Types.FLIGHT, timeAcumulated, timestamp));
		timeAcumulated += timestamp;
	}

	//this wait will be done by C#
	public double GetTrackTimeInSecondsAndEmptyLists()
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

	//private methods --------------------------------------
	
	private double getDCBiggestTC()
	{
		int bigTCPosition = rpim.GetPosOfBiggestTC();
		double sum = rpim.SumUntilPos(bigTCPosition);

		LogB.Information(string.Format("\n----------------\ngetDCBiggestTC, list: {0}, bigTCPosition: {1}, sum: {2}", rpim.PrintList(), bigTCPosition, sum));

		if(sum < checkTime)
		{
			while (sum < checkTime && bigTCPosition +2 <= rpim.LastPositionOfList)
			{
				bigTCPosition += 2;
				sum = rpim.SumUntilPos(bigTCPosition);
				LogB.Information(string.Format("SUM was < checkTime. New bigTCPosition: {0}, New Sum: {1}", bigTCPosition, sum));
			}
		}

		rpim.EmtpyListUntilPos(bigTCPosition);

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
		foreach(PhaseTime pt in listPhaseTime)
			str += pt.ToString();

		return str;
	}

	public List<string> InListForPainting()
	{
		List<string> list_in = new List<string>();
		int currentMS = 0;
		int startInMS = -1;
		foreach(PhaseTime pt in listPhaseTime)
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
		string report = string.Format("Report of race started at: {0}; ended at: {1}", dtStarted.ToShortTimeString(), dtEnded.ToShortTimeString());
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
