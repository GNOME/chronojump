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

/*
   RUN OBJECTS
 */

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

//decide if use this or inspector
//TODO: clarify what this class does
public class RunPhaseTimeList
{
	public bool SpeedStart;

	private List<PhaseTime> listPhaseTime;
	private Constants.DoubleContact checkDoubleContactMode;
	private int checkTime;

	//if there are double contacts at start, first run phase infos will not be used
	public int FirstRPIs;

	public RunPhaseTimeList(Constants.DoubleContact checkDoubleContactMode, int checkTime)
	{
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkTime = checkTime;

		listPhaseTime = new List<PhaseTime>();
		FirstRPIs = 0;
	}
	
	public void AddTC(int photocell, double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(photocell, true, timestamp));
	}

	public void AddTF(int photocell, double timestamp)
	{
		listPhaseTime.Add(new PhaseTime(photocell, false, timestamp));
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

	//to show tc chunks or not on gui/eventExecute.cs
	public bool UseDoubleContacts()
	{
		return (checkDoubleContactMode != Constants.DoubleContact.NONE);
	}

	public List<RunPhaseTimeListObject> InListForPainting()
	{
		List<RunPhaseTimeListObject> list_in = new List<RunPhaseTimeListObject>();
		int currentMS = 0;
		int startInMS = -1;
		int startInPhotocell = -1;
		int currentPhotocell = -1;

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

		// 3) add elements to the list
		LogB.Information("InListForPainting foreach:");
		int count = 0;
		double negativeValues = 0; //double contacts times at start
		PhaseTime ptLast = null;

		RunPhaseTimeListObject.Phases currentPhase = RunPhaseTimeListObject.Phases.START;
		RunPhaseTimeListObject rptloToAdd = null;

		foreach(PhaseTime pt in listPhaseTimeShallowCloned)
		{
			LogB.Information(pt.ToString());

			if(FirstRPIs > count)
			{
				negativeValues += pt.Duration/1000.0;
				LogB.Information("InListForPainting negativeValues = " + negativeValues.ToString());
			}

			if(pt.IsContact) {
				startInMS = currentMS;
				startInPhotocell = pt.Photocell;
			}
			else if(startInMS >= 0)
			{
				//see if previous has ended to mark as END or STARTEND
				if(rptloToAdd != null)
				{
					bool thisPhaseEnds = false;
					if(list_in.Count == 0 && ! SpeedStart)
					{
						//on ! speedStart first tc+tf pair, count only tf
						if(startInMS/1000.0 - rptloToAdd.tcEnd > checkTime/1000.0)
							thisPhaseEnds = true;
					}
					else if(startInMS/1000.0 - rptloToAdd.tcStart > checkTime/1000.0)
						thisPhaseEnds = true;

					if(thisPhaseEnds)
					{
						if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START)
							rptloToAdd.phase = RunPhaseTimeListObject.Phases.STARTANDEND;
						else
							rptloToAdd.phase = RunPhaseTimeListObject.Phases.END;

						currentPhase = RunPhaseTimeListObject.Phases.START;
					} else
						currentPhase = RunPhaseTimeListObject.Phases.MIDDLE;

					list_in.Add(rptloToAdd);
				}

				//this will be added in next iteration of flight (! pt.IsContact)
				rptloToAdd = new RunPhaseTimeListObject(
						currentPhase,
						startInMS/1000.0,
						currentMS/1000.0,
						startInPhotocell,
						pt.Photocell);
			}

			currentMS += Convert.ToInt32(pt.Duration);
			currentPhotocell = pt.Photocell;

			LogB.Information(string.Format("End of iteration: {0}, pt.IsContact: {1}, startInMS: {2}, currentMS: {3}",
						count, pt.IsContact, startInMS, currentMS));

			ptLast = pt;
			count ++;
		}

		//add pending rptl
		if(startInMS/1000.0 - rptloToAdd.tcStart > checkTime/1000.0)
		{
			if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START)
				rptloToAdd.phase = RunPhaseTimeListObject.Phases.STARTANDEND;
			else
				rptloToAdd.phase = RunPhaseTimeListObject.Phases.END;
		}

		list_in.Add(rptloToAdd);

		//when track ends, last phase is a TC, add it
		if(ptLast != null && ptLast.IsContact)
		{
			RunPhaseTimeListObject rptloLast = new RunPhaseTimeListObject(
						RunPhaseTimeListObject.Phases.STARTANDEND,
						startInMS/1000.0,
						(startInMS + ptLast.Duration)/1000.0,
						startInPhotocell, ptLast.Photocell);

			if(rptloToAdd.phase == RunPhaseTimeListObject.Phases.START ||
					rptloToAdd.phase == RunPhaseTimeListObject.Phases.MIDDLE)
				rptloLast.phase = RunPhaseTimeListObject.Phases.END;

			list_in.Add(rptloLast);
		}

		//manage the negative values
		if(negativeValues > 0)
		{
			LogB.Information("Fixing negative values (double contacts times at start)");
			for (int i = 0; i < list_in.Count; i ++)
			{
				LogB.Information(string.Format("PRE i: {0}, list_in[{0}]: {1}", i, list_in[i]));

				RunPhaseTimeListObject rptlo = (RunPhaseTimeListObject) list_in[i];
				rptlo.tcStart -= negativeValues;
				rptlo.tcEnd -= negativeValues;
				list_in[i] = rptlo;
			}
		}

		return list_in;
	}

	//Debug
	public string InListForPaintingToString()
	{
		string str = "Contact in time list:\n";
		List<RunPhaseTimeListObject> list_in = InListForPainting();
		foreach(RunPhaseTimeListObject rptlo in list_in)
			str += rptlo.ToString() + "\n";

		return str;
	}

	public List<PhaseTime> ListPhaseTime
	{
		get { return listPhaseTime; }
	}

}

//TODO: clarify what this class does
public class RunPhaseTimeListObject
{
	//each contact can be start of a chunk, middle, end or startandend
	//this is important for the drawing in gui/eventExecute.cs
	public enum Phases { START, MIDDLE, END, STARTANDEND }
	public Phases phase;
	public double tcStart;
	public double tcEnd;
	public int photocellStart;
	public int photocellEnd;


	public RunPhaseTimeListObject ()
	{
	}

	public RunPhaseTimeListObject (Phases phase,
			double tcStart, double tcEnd, int photocellStart, int photocellEnd)
	{
		this.phase = phase;
		this.tcStart = tcStart;
		this.tcEnd = tcEnd;
		this.photocellStart = photocellStart;
		this.photocellEnd = photocellEnd;
	}

	public override string ToString()
	{
		return phase.ToString() + ":" +
			Math.Round(tcStart, 3).ToString() + ":" +
			Math.Round(tcEnd, 3).ToString() + ":" +
			photocellStart.ToString() + ":" +
			photocellEnd.ToString();
	}
}

//TODO: clarify what this class does
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

	//photocell = -1 for not inalambric photocells
	public void ChangePhase(int photocell, Phases phase)
	{
		ChangePhase(photocell, phase, "");
	}
	public void ChangePhase(int photocell, Phases phase, string message)
	{
		DateTime dt = DateTime.Now;

		if(phase == Phases.START)
			dtStarted = dt;
		else if(phase == Phases.END)
			dtEnded = dt;
		else // (phase == Phases.IN || phases == Phases.OUT)
		{
			InOut inOut = new InOut(photocell, phase == Phases.IN, dt, message);
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

/*
   This is written by capture thread and readed by GTK thread.
   Manages the icon shown on running.
   Image will be a person RUNNING or a PHOTOCELL being shown (when cross it).
   Also on wireless, show the number of the photocell
   */

public class RunChangeImage
{
	public enum Types { NONE, RUNNING, PHOTOCELL }
	private Types last;
	private Types current;
	private int photocell; //0 is a valid value

	//constructor, don't show any image
	public RunChangeImage()
	{
		last = Types.NONE;
		current = Types.NONE;
		photocell = -1;
	}

	public bool ShouldBeChanged()
	{
		if(current == last)
			return false;

		last = current;
		return true;
	}

	//accesssor: get/change current image
	public Types Current {
		get { return current; }
		set { current = value; }
	}

	public int Photocell {
		get { return photocell; }
		set { photocell = value; }
	}

}

/*
   JUMP OBJECTS
 */

/*
   This is written by capture thread and readed by GTK thread.
   Manages the icon shown on jumping.
   */

public class JumpChangeImage
{
	public enum Types { NONE, AIR, LAND }
	private Types last;
	private Types current;

	//constructor, don't show any image
	public JumpChangeImage()
	{
		last = Types.NONE;
		current = Types.NONE;
	}

	public bool ShouldBeChanged()
	{
		//LogB.Information (string.Format ("ShouldBeChanged, current: {0}, last: {1}", current, last));
		if(current == last)
			return false;

		last = current;
		return true;
	}

	//accesssor: get/change current image
	public Types Current {
		get { return current; }
		set { current = value; }
	}
}
