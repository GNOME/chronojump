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
	public bool TrackDoneHasToBeCalledAgain;
	private List<RunPhaseInfo> list;

	//TCs and TFs before startPos have been added as tracks
	//do not count again in track operations
	private int startPos;

	public RunPhaseInfoManage ()
	{
		list = new List<RunPhaseInfo>();
		startPos = 0;
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
	 */
	public bool IsStartDoubleContact(int checkTime)
	{
		LogB.Information("At rpim IsStartDoubleContact A");
		int startAt = 0;

		//first TF if exists
		if(list.Count > 0)
		{
			RunPhaseInfo firstRPI = (RunPhaseInfo) list[0];
			if(! firstRPI.IsContact()) //is TF
			{
				if(firstRPI.Duration > checkTime)
					return false;

				startAt = 1;
			}
		}
		LogB.Information("At rpim IsStartDoubleContact B");

		//TC+TF pairs
		RunPhaseInfo tcRPI;
		RunPhaseInfo tfRPI;
		for(int i = startAt +1; i < list.Count; i +=2)
		{
			tcRPI = (RunPhaseInfo) list[i-1];
			tfRPI = (RunPhaseInfo) list[i];

			LogB.Information("At rpim IsStartDoubleContact C");
			if(tcRPI.Duration + tfRPI.Duration > checkTime)
				return false;
		}
		LogB.Information("At rpim IsStartDoubleContact D");

		return true;
	}

	/*
	 * check double contacts at start:
	 * if ! speedStart: start at contact first contact will be TF (race starts at beginning)
	 * if   speedStart: start before contact (race starts related to biggest_TC
	 * 	if(speedStartArrival) 	first contact will be TC (start at beginning of biggest_TC)
	 * 	else 			first contact will be TF (start at end of biggest_TC)
	 */
		/*
		 * aixo sera la funció start double contact position()
		 * maybe not used because GetPosOfBiggestTC will be recicled
		 *
		 *
	//return the double contact tc+tf at beginning
		if(! speedStart)
			return 0;

		//--- from now on speedStart

		//bool first = true;
		int startAt = 0;

		double maxTCDuration = 0;
		int maxTCPosition = 0;
		bool speedStartArrival = true;

		if(list.Count > 0)
		{
			RunPhaseInfo firstRPI = (RunPhaseInfo) list[0];
			if(! firstRPI.IsContact()) //is TF
			{
				speedStartArrival = false;
				if (firstRPI.Duration < checkTime)
					startAt = 1;
				else
					return 0;
			}
		}

		RunPhaseInfo tcRPI;
		RunPhaseInfo tfRPI;
		for(int i = startAt +1; i < list.Count; i +=2)
		{
			tcRPI = (RunPhaseInfo) list[i-1];
			tfRPI = (RunPhaseInfo) list[i];

			if(tcRPI.Duration > maxTCDuration)
			{
				maxTCDuration = tcRPI.Duration;
				maxTCPosition = i;
			}

			if(tcRPI.Duration + tfRPI.Duration >= checkTime)
			{
				if(speedStartArrival)
					return maxTCPosition -1;
				else
					return maxTCPosition;
			}
		}

		if(speedStartArrival)
			return maxTCPosition -1;
		else
			return maxTCPosition;
	}
	*/

	public int GetPosOfBiggestTC (bool started, int checkTime)
	{
		LogB.Information("startPos at GetPosOfBiggestTC: " + startPos.ToString());
		TrackDoneHasToBeCalledAgain = false;

		//Read below message: "Message oneTCAfterTheTf"
		if(countTCs() == 1 && oneTCAfterTheTf())
			return startPos +1;

		double max = 0;
		int pos = 0;
		int posBiggest = 0;
		double lastTcDuration = 0;

		foreach(RunPhaseInfo rpi in list)
		{
			/*
			 * first time we need to know if first TC is greater than the others
			 * but once started, we care for endings of each track,
			 * do not use the first value because it's the TC of previous track
			 */
			if( (started && pos >= startPos +1) || (! started && pos >= startPos) )
			{
				/*
				 * record tc duration as lastTcDuration and add to td duration to see if is greater than checktime
				 * this allows to return biggest_tc of one track without messing with next track that maybe is captured
				 * this happens because double contacts is eg: 300 and trackDone is calle at 300 * 1,5
				 * But then trackDone has to be called again!
				 */
				if(rpi.IsContact())
					lastTcDuration = rpi.Duration;
				else if(! rpi.IsContact() && lastTcDuration + rpi.Duration > checkTime)
				{
					//check if there's another track in this set
					for(int i = pos + 2; i < list.Count; i += 2)
					{
						RunPhaseInfo tcRPI = (RunPhaseInfo) list[i-1];
						RunPhaseInfo tfRPI = (RunPhaseInfo) list[i];

						if(tcRPI.Duration + tfRPI.Duration > checkTime)
							TrackDoneHasToBeCalledAgain = true;
					}

					return posBiggest;
				}

				//record posBiggest position
				if(rpi.IsContact() && rpi.Duration > max)
				{
					max = rpi.Duration;
					posBiggest = pos;
				}
			}

			pos ++;
		}
		return posBiggest;
	}

	//if pos == -1 return all
	public double SumUntilPos(int pos, bool firstTrackDone, bool speedStartArrival)
	//public double SumUntilPos(int pos)
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
				//if it has not firstTrackDone 1st track take care of leaving or not to count the related tc)
				if(! firstTrackDone && sum == 0 && rpi.IsContact() && ! speedStartArrival)
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
		rpim = new RunPhaseInfoManage();
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

	public void DoneTC (double timestamp, bool timeStarted)
	{
		LogB.Information("DONETC timestamp: " + timestamp + timestamp.ToString());
		lastTc = timestamp;
		listCaptureThread.Add(new RunPhaseInfo(RunPhaseInfo.Types.CONTACT, timeAcumulated, timestamp));
		if(timeStarted)
			timeAcumulated += timestamp;

		LogB.Information(string.Format("DoneTC -> lastTc: {0}", lastTc));
	}

	public void DoneTF (double timestamp)
	{
		LogB.Information("DONETF timestamp: " + timestamp + timestamp.ToString());
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

	public bool IsStartDoubleContact()
	{
		LogB.Information("At RunDC IsStartDoubleContact");
		//return rpim.IsStartDoubleContact(checkTime);

		bool isDC = rpim.IsStartDoubleContact(checkTime);
		LogB.Information("IsStartDoubleContact: " + isDC.ToString());
		return isDC;
	}

	public int GetPosOfBiggestTC(bool started)
	{
		int pos = rpim.GetPosOfBiggestTC(started, checkTime);

		if(rpim.TrackDoneHasToBeCalledAgain)
		{
			TrackDoneHasToBeCalledAgain = true;
			//rpim.TrackDoneHasToBeCalledAgain = false;
		}

		LogB.Information(string.Format("GetPosOfBiggestTC list: {0}, pos: {1}, hasToBeCaledAgain: {2}",
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

	/*
	 * <---------------------- end of called by GTK thread --------------
	 */

	//private methods --------------------------------------
	
	private double getDCBiggestTC()
	{
		int bigTCPosition = GetPosOfBiggestTC(true);
		double sum = rpim.SumUntilPos(bigTCPosition, FirstTrackDone, speedStartArrival);
		LogB.Information(string.Format("trackDoing getDBBiggestTC bigTCPosition: {0}, Sum: {1}", bigTCPosition, sum));

		//fix problem of a tc + tf lower than checkTime
		if(sum < checkTime)
		{
			while (sum < checkTime && bigTCPosition +2 <= rpim.LastPositionOfList)
			{
				bigTCPosition += 2;
				sum = rpim.SumUntilPos(bigTCPosition, FirstTrackDone, speedStartArrival);
				LogB.Information(string.Format("SUM was < checkTime. New bigTCPosition: {0}, New Sum: {1}", bigTCPosition, sum));
			}
		}

		UpdateStartPos(bigTCPosition);

		return sum;
	}
}

//decide if use this or inspector
public class RunPhaseTimeList
{
	private List<PhaseTime> listPhaseTime;

	//if there are double contacts at start, first run phase infos will not be used
	public int FirstRPIs;

	public RunPhaseTimeList()
	{
		listPhaseTime = new List<PhaseTime>();
		FirstRPIs = 0;
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

		// 1) create a copy of listPhaseTime in order to do foreach without problems with other thread that adds records
		//This is problematic (Collection was modified; enumeration operation may not execute) if other thread is changing it:
		//foreach(PhaseTime pt in listPhaseTime)
		//solution:
		List<PhaseTime> listPhaseTimeShallowCloned = new List<PhaseTime>(listPhaseTime);

		/*
		 * 2) check if we started in because 1st TC has to be counted in the track
		 * but 2nd TC has to be the end of the first track
		 * we need this to synchronize correctly
		 */
//		bool startedIn = false;
		if(listPhaseTimeShallowCloned.Count >= 1)
		{
			PhaseTime ptFirst = (PhaseTime) listPhaseTimeShallowCloned[0];
//		if(ptFirst.IsContact)
//				startedIn = true;
		}

		//if FirstRPIs is 1 then inverted the startedIn
//		if(FirstRPIs == 1)
//			startedIn = ! startedIn;

		// 3) add elements to the list
		LogB.Information("InListForPainting foreach:");
		int count = 0;
		double negativeValues = 0; //double contacts times at start
		PhaseTime ptLast = null;
		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
		{
			LogB.Information(pt.ToString());

			if(FirstRPIs > count)
			{
				negativeValues += pt.Duration/1000.0;
				LogB.Information("InListForPainting negativeValues = " + negativeValues.ToString());
			}

			if(pt.IsContact)
				startInMS = currentMS;
			else if(startInMS >= 0)
				list_in.Add(startInMS/1000.0 + ":" + currentMS/1000.0); //in seconds

			currentMS += Convert.ToInt32(pt.Duration);

			LogB.Information(string.Format("End of iteration: {0}, pt.IsContact: {1}, startInMS: {2}, currentMS: {3}",
						count, pt.IsContact, startInMS, currentMS));

			ptLast = pt;
			count ++;
		}

		//when track ends, last phase is a TC, add it
		if(ptLast != null && ptLast.IsContact)
			list_in.Add( startInMS/1000.0 + ":" +
					(startInMS + ptLast.Duration)/1000.0); //in seconds

		//manage the negative values
		if(negativeValues > 0)
		{
			LogB.Information("Fixing negative values (double contacts times at start)");
			for (int i = 0; i < list_in.Count; i ++)
			{
				LogB.Information(string.Format("PRE i: {0}, list_in[{0}]: {1}", i, list_in[i]));

				string [] strFull = list_in[i].Split(new char[] {':'});
				list_in[i] = (Convert.ToDouble(strFull[0]) - negativeValues).ToString() + ":" +
					(Convert.ToDouble(strFull[1]) - negativeValues).ToString();

				LogB.Information(string.Format("POST i: {0}, list_in[{0}]: {1}", i, list_in[i]));
			}
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
